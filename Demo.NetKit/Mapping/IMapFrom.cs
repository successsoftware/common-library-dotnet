using AutoMapper;

namespace Demo.NetKit.Mapping
{
    public interface IMapFrom<T>
    {
        void Mapping(Profile profile);
    }
}
