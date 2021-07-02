using System.Threading.Tasks;

namespace API.Interface
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository {get;}
        IMessageRepository MessageRepository{get;}
        ILikesRepository LikesRespository{get;}
        Task<bool> Complete();
        bool HasChanges();
    }
}