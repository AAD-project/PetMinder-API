using Api.DTOs;
using Api.Models;
using AutoMapper;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User Mappings
        CreateMap<User, UserResponseDto>()
            .ForMember(dest => dest.Pets, opt => opt.MapFrom(src => src.Pets))
            .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.Tasks))
            .ForMember(dest => dest.Reminders, opt => opt.MapFrom(src => src.Reminders));

        CreateMap<UserCreateRequestDto, User>();

        // **Handle update mappings explicitly**
        CreateMap<UserUpdateRequestDto, User>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

        // Pet Mappings
        CreateMap<Pet, PetResponseDto>();
        CreateMap<PetCreateRequestDto, Pet>();

        // HealthData Mappings
        CreateMap<HealthData, HealthDataResponseDto>();

        // Reminder Mappings
        CreateMap<Reminder, ReminderResponseDto>();
        CreateMap<ReminderCreateRequestDto, Reminder>();

        // TodoTask Mappings
        CreateMap<TodoTask, TodoTaskResponseDto>();
        CreateMap<TodoTaskCreateRequestDto, TodoTask>();
    }
}
