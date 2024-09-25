using AutoMapper;
using DrawOutApp.Server.Entities;
using DrawOutApp.Server.Models;

namespace DrawOutApp.Server.Mappers
{
    public class GameMapper : Profile
    {
        public GameMapper()
        {
            CreateMap<Game, GameModel>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => src._id))
                .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.RoomId))
                .ForMember(dest => dest.TotalRounds, opt => opt.MapFrom(src => src.TotalRounds))
                .ForMember(dest => dest.PainterOrder, opt => opt.MapFrom(src => src.PainterOrder))
                .ForMember(dest => dest.TeamLeaders, opt => opt.MapFrom(src => src.TeamLeaders));

            
            CreateMap<GameModel, Game>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => src._id))
                .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.RoomId))
                .ForMember(dest => dest.TotalRounds, opt => opt.MapFrom(src => src.TotalRounds))
                .ForMember(dest => dest.PainterOrder, opt => opt.MapFrom(src => src.PainterOrder))
                .ForMember(dest => dest.TeamLeaders, opt => opt.MapFrom(src => src.TeamLeaders))
                .ForMember(dest => dest.MainTimer, opt => opt.MapFrom(src => src.MainTimer))
                .ForMember(dest => dest.StealTimer, opt => opt.MapFrom(src => src.StealTimer))
                .ForMember(dest => dest.GameState, opt => opt.Ignore())
                .ForMember(dest => dest.BlueScore, opt => opt.Ignore())
                .ForMember(dest => dest.RedScore, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentRound, opt => opt.Ignore())
                .ForMember(dest => dest.CurrentPainter, opt => opt.Ignore())
                .ForMember(dest => dest.SelectedWord, opt => opt.Ignore());

            CreateMap<Game, GameRoundModel>()
                .ForMember(dest => dest._gameId, opt => opt.MapFrom(src => src._id))
                .ForMember(dest => dest.GameState, opt => opt.MapFrom(src => src.GameState.ToString()))
                .ForMember(dest => dest.BlueScore, opt => opt.MapFrom(src => src.BlueScore))
                .ForMember(dest => dest.RedScore, opt => opt.MapFrom(src => src.RedScore))
                .ForMember(dest => dest.CurrentRound, opt => opt.MapFrom(src => src.CurrentRound))
                .ForMember(dest => dest.CurrentPainter, opt => opt.MapFrom(src => src.CurrentPainter))
                .ForMember(dest => dest.SelectedWord, opt => opt.MapFrom(src => src.SelectedWord));
               

            CreateMap<GameRoundModel, Game>()
                .ForMember(dest => dest._id, opt => opt.MapFrom(src => src._gameId))
                .ForMember(dest => dest.GameState, opt => opt.MapFrom(src => Enum.Parse<GameState>(src.GameState!)))
                .ForMember(dest => dest.BlueScore, opt => opt.MapFrom(src => src.BlueScore))
                .ForMember(dest => dest.RedScore, opt => opt.MapFrom(src => src.RedScore))
                .ForMember(dest => dest.CurrentRound, opt => opt.MapFrom(src => src.CurrentRound))
                .ForMember(dest => dest.CurrentPainter, opt => opt.MapFrom(src => src.CurrentPainter))
                .ForMember(dest => dest.SelectedWord, opt => opt.MapFrom(src => src.SelectedWord))
                .ForMember(dest => dest.MainTimer, opt => opt.Ignore())
                .ForMember(dest => dest.StealTimer, opt => opt.Ignore())
                .ForMember(dest => dest.RoomId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalRounds, opt => opt.Ignore())
                .ForMember(dest => dest.PainterOrder, opt => opt.Ignore())
                .ForMember(dest => dest.TeamLeaders, opt => opt.Ignore());
        }
    }
}
