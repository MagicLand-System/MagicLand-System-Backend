using MagicLand_System.Background.BackgroundServiceInterfaces;
using MagicLand_System.Domain.Models;
using MagicLand_System.Domain;
using MagicLand_System.Repository.Interfaces;
using MagicLand_System.Domain.Models.TempEntity.Quiz;
using MagicLand_System.Domain.Models.TempEntity.Class;

namespace MagicLand_System.Background.BackgroundServiceImplements
{
    public class TempEntityBackgroundService : ITempEntityBackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public TempEntityBackgroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<string> DeleteTempEntityByCondition()
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<MagicLandContext>>();
                    var currentDate = DateTime.Now;

                    var tempQuiz = await _unitOfWork.GetRepository<TempQuiz>().GetListAsync(orderBy: x => x.OrderBy(x => x.CreatedTime));

                    foreach (var quiz in tempQuiz)
                    {
                        int time = currentDate.Day - quiz.CreatedTime.Day;

                        if (time >= 1)
                        {
                            _unitOfWork.GetRepository<TempQuiz>().DeleteAsync(quiz);
                        }
                    }
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                return $"Delete Temp Entity Got An Error: [{ex.Message}]";
            }
            return "Delete Temp Entity Success";
        }

        public async Task<string> UpdateTempItemPrice()
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork<MagicLandContext>>();
                    var currentDate = DateTime.Now;

                    var tempItemPrices = await _unitOfWork.GetRepository<TempItemPrice>().GetListAsync();

                    foreach (var item in tempItemPrices)
                    {
                        if (item.ClassId != default)
                        {
                            double result = 0;

                            var cls = await _unitOfWork.GetRepository<Class>().SingleOrDefaultAsync(predicate: x => x.Id == item.ClassId);

                            var coursePrices = (await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                                selector: x => x.CoursePrices,
                                predicate: x => x.Id == cls.CourseId))!.ToArray();

                            coursePrices = coursePrices.OrderBy(x => x.EffectiveDate).ToArray();

                            result = coursePrices.First().Price;
                            for (int i = 1; i < coursePrices.Length; i++)
                            {
                                if (cls.AddedDate >= coursePrices[i].EffectiveDate)
                                {
                                    result = coursePrices[i].Price;
                                }
                                else
                                {
                                    break;
                                }
                            }

                            item.Price = result;
                        }
                        if (item.CourseId != default)
                        {
                            double result = 0;

                            var coursePrices = await _unitOfWork.GetRepository<Course>().SingleOrDefaultAsync(
                                selector: x => x.CoursePrices,
                                predicate: x => x.Id == item.CourseId);

                            coursePrices = coursePrices!.OrderByDescending(x => x.EffectiveDate).ToList();

                            result = coursePrices.Last().Price;

                            var currentValidPrice = coursePrices.FirstOrDefault(x => x.EffectiveDate <= DateTime.Now)?.Price;

                            if (currentValidPrice is not null)
                            {
                                result = currentValidPrice.Value;
                            }

                            item.Price = result;
                        }
                    }

                    _unitOfWork.GetRepository<TempItemPrice>().UpdateRange(tempItemPrices);
                    _unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                return $"Delete Temp Entity Got An Error: [{ex.Message}]";
            }
            return "Delete Temp Entity Success";
        }
    }
}
