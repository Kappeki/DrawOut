using Amazon.Runtime;
using AutoMapper;
using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Mappers
{
    public class UserMapper : Profile
    {
        public UserMapper()
        {
            CreateMap<UserModel, User>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                src.Roles != null ?
                new HashSet<Role>(src.Roles.Select(r => Enum.Parse<Role>(r))) :
                new HashSet<Role>()));

            CreateMap<User, UserModel>()
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => 
                src.Roles != null ? 
                src.Roles.Select(r => r.ToString()).ToList() : 
                new List<string>()));

            CreateMap<User, PlayerInfo>()
                .ForMember(dest => dest.Nickname, opt => opt.MapFrom(src => src.Nickname))
                .ForMember(dest => dest.Icon, opt => opt.MapFrom(src => src.Icon));
        }
    }
}
