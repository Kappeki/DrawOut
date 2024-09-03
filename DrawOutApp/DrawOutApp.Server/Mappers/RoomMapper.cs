using AutoMapper;
using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;
using MongoDB.Bson;
using System.Text.RegularExpressions;

namespace DrawOutApp.Server.Mappers
{
    public class RoomMapper : Profile
    {
        public RoomMapper()
        {
            CreateMap<Room, RoomModel>()
                .ForMember(dest => dest.RoomState, opt => opt.MapFrom(src => src.RoomState.ToString()))
                .ForMember(dest => dest.RoundTime, opt => opt.MapFrom(src => (int)src.RoundTime))
                .ForMember(dest => dest.PasswordHash, opt=>opt.MapFrom(src => src.Password))
                .ForMember(dest => dest.Players, opt => opt.Ignore());

            CreateMap<RoomModel, Room>()
                .ForMember(dest => dest.RoomState, opt => opt.MapFrom(src => Enum.Parse<GameState>(src.RoomState)))
                .ForMember(dest => dest.RoundTime, opt => opt.MapFrom(src => (RoundTime)src.RoundTime))
                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.PasswordHash))
                .ForMember(dest => dest._id, opt => opt.Ignore())
                .ForMember(dest => dest.ObjectId, opt => opt.Ignore())
                .ForMember(dest => dest.PlayersSetKey,opt => opt.Ignore());

            CreateMap<Room, RoomListItem>()
                .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.ObjectId))
                .ForMember(dest => dest.HasPassword, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Password)))
                .ForMember(dest => dest.RoomState, opt => opt.MapFrom(src => src.RoomState.ToString()));
        }
        
    }
}
