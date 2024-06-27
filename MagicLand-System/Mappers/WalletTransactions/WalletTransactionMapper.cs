using AutoMapper;
using MagicLand_System.Domain.Models;
using MagicLand_System.Mappers.Custom;
using MagicLand_System.PayLoad.Response.Users;
using MagicLand_System.PayLoad.Response.WalletTransactions;

namespace MagicLand_System.Mappers.WalletTransactions
{
    public class WalletTransactionMapper : Profile
    {
        public WalletTransactionMapper()
        {
            CreateMap<WalletTransaction, WalletTransactionResponse>()
           .ForMember(dest => dest.Parent, opt => opt.MapFrom(src => UserCustomMapper.fromUserToUserResponse(src.PersonalWallet!.User!)))
           .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
           .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
           .ForMember(dest => dest.CreatedTime, opt => opt.MapFrom(src => src.CreateTime))
           .ForMember(dest => dest.Money, opt => opt.MapFrom(src => src.Money))
           .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method))
           .ForMember(dest => dest.TransactionCode, opt => opt.MapFrom(src => src.TransactionCode))
           .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
           .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
           .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.Discount));
        
        }
    }
}
