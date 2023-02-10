using AutoMapper;

namespace Demo.NetKit.Mapping
{
    public interface IMapTo<T>
    {
        void Mapping(Profile profile);
    }
}
