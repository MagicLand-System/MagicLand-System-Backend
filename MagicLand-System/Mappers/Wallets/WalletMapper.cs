using AutoMapper;
using MagicLand_System.Domain.Models;
using MagicLand_System.PayLoad.Response.Topics;
using MagicLand_System.PayLoad.Response.WalletTransactions;

namespace MagicLand_System.Mappers.Wallets
{
    public class WalletMapper : Profile
    {
        public WalletMapper()
        {
            CreateMap<PersonalWallet, WalletResponse>()
              .ForMember(dest => dest.Owner, opt => opt.MapFrom(src => src.User!.FullName))
              .ForMember(dest => dest.Balance, opt => opt.MapFrom(src => src.Balance));

        }
    }
}
