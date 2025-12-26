using AutoMapper;
using DexRobotPDA.DataModel;
using DexRobotPDA.DTOs;

namespace DexRobotPDA.AutoMappers;

public class AutoMapperSetting : Profile
{
    public AutoMapperSetting()
    {
        // byte转bool：仅1为true，0为false，其他值也视为false
        CreateMap<byte, bool>().ConvertUsing(src => src == 1);

        // bool转byte：true→1，false→0
        CreateMap<bool, byte>().ConvertUsing(src => src ? (byte)1 : (byte)0);

        CreateMap<EmployeeModel, EmployeeDto>().ReverseMap();
        CreateMap<ProductTaskModel, ProductTaskDto>().ReverseMap();
        CreateMap<MotorModel, MotorDto>().ReverseMap();
        CreateMap<ServoModel, ServoDto>().ReverseMap();
        CreateMap<FingerModel, FingerDto>().ReverseMap();
        CreateMap<PalmModel, PalmDto>().ReverseMap();
        CreateMap<MotorWormDetectModel, MotorWormDetectDto>().ReverseMap();
        CreateMap<SplitWormDetectModel,SplitWormDetectDto>().ReverseMap();
        CreateMap<SplitCalibrateDetectModel, SplitCalibrateDetectDto>().ReverseMap();
        CreateMap<FingerCalibrateDetectModel, FingerCalibrateDetectDto>().ReverseMap();
        CreateMap<PalmCalibrateDetectModel, PalmCalibrateDetectDto>().ReverseMap();
        CreateMap<EventLogModel, EventLogDto>().ReverseMap();
        CreateMap<AddTaskDto, ProductTaskModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore());

        CreateMap<EmployeeModel, UserDto>();

        CreateMap<AddMotorDto, MotorModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore());
        
        CreateMap<AddServoDto, ServoModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore());

        CreateMap<AddFingerDto, FingerModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore());

        CreateMap<AddPalmDto, PalmModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore());
        
        CreateMap<MotorDetectCreateDto, MotorWormDetectModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore());
        
        CreateMap<SplitWormDetectCreateDto,SplitWormDetectModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore());
        
        CreateMap<SplitCalibrateDetectCreateDto,SplitCalibrateDetectModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore());
        
        CreateMap<FingerCalibrateDetectCreateDto,FingerCalibrateDetectModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore());
        
        CreateMap<PalmCalibrateDetectCreateDto,PalmCalibrateDetectModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore());
                
        CreateMap<AddEventLogDto,EventLogModel>()
            .ForMember(dest => dest.id, opt => opt.Ignore());
    }
}