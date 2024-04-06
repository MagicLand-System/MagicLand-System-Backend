using MagicLand_System.Background;
using MagicLand_System.Background.BackgroundServiceImplements;
using MagicLand_System.Background.BackgroundServiceInterfaces;
using MagicLand_System.Config;
using MagicLand_System.Domain;
using MagicLand_System.Middlewares;
using MagicLand_System.Repository.Implement;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Services.Implements;
using MagicLand_System.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NgrokAspNetCore;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo() { Title = "MagicLand System", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
      {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
      });
    options.MapType<TimeOnly>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "time",
        Example = OpenApiAnyFactory.CreateFromJson("\"13:45:42.0000000\"")
    });


    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.Configure<VnpayConfig>(builder.Configuration.GetSection(VnpayConfig.ConfigName));
builder.Services.Configure<FcmNotificationSetting>(builder.Configuration.GetSection(FcmNotificationSetting.ConfigName));
builder.Services.AddDbContext<MagicLandContext>();
//builder.Services.AddScoped<MagicLandContext>();
builder.Services.AddScoped<IUnitOfWork<MagicLandContext>, UnitOfWork<MagicLandContext>>();
builder.Services.AddControllers(opt => opt.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true).AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "MyDefaultPolicy",
        policy => { policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod(); });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IGatewayService, GatewayService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<ISlotService, SlotService>();
builder.Services.AddScoped<IWalletTransactionService, WalletTransactionService>();
builder.Services.AddScoped<IPersonalWalletService, PersonalWalletService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ISyllabusService, SyllabusService>();

var quartzJobs = builder.Configuration.GetSection("QuartzJobs").GetChildren();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Services.AddScoped<IClassBackgroundService, ClassBackgroundService>();
builder.Services.AddScoped<ITransactionBackgroundService, TransactionBackgroundService>();
builder.Services.AddScoped<INotificationBackgroundService, NotificationBackgroundService>();
builder.Services.AddScoped<ITempEntityBackgroundService, TempEntityBackgroundService>();
builder.Services.AddHostedService<QuartzScheduler>();
builder.Services.AddSingleton<IJobFactory, JobFactoryService>();
builder.Services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
builder.Services.AddScoped<DailyUpdateJob>();
builder.Services.AddScoped<DailyCreateJob>();
builder.Services.AddScoped<DailyDeleteJob>();
builder.Services.AddSingleton(new Job(type: typeof(DailyUpdateJob), expression: DetermineCronExpression(quartzJobs, "DailyUpdateJob")));
builder.Services.AddSingleton(new Job(type: typeof(DailyCreateJob), expression: DetermineCronExpression(quartzJobs, "DailyCreateJob")));
builder.Services.AddSingleton(new Job(type: typeof(DailyDeleteJob), expression: DetermineCronExpression(quartzJobs, "DailyDeleteJob")));

builder.Services.AddNgrok();

var app = builder.Build();


app.UseSwagger();

app.UseSwaggerUI(c =>
{
    if (env.IsDevelopment() || env.IsProduction())
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web API V1");
    }
    else
    {
        c.SwaggerEndpoint("/somee/swagger/v1/swagger.json", "Web API V1");
    }

});

app.UseCors("MyDefaultPolicy");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.Run();
string DetermineCronExpression(IEnumerable<IConfigurationSection> quartzJobs, string jobName)
{
    var selectedJob = quartzJobs.FirstOrDefault(job => job["JobName"] == jobName);
    return selectedJob?["CronExpression"] ?? "0/5 * * * * ?";
}
