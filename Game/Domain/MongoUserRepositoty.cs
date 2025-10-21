using System;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Game.Domain
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserEntity> userCollection;
        public const string CollectionName = "users";

        public MongoUserRepository(IMongoDatabase database)
        {
            userCollection = database.GetCollection<UserEntity>(CollectionName);
            userCollection.Indexes
                .CreateOne(Builders<UserEntity>.IndexKeys.Ascending("login"), new CreateIndexOptions { Unique = true });
        }

        public UserEntity Insert(UserEntity user)
        {
            userCollection.InsertOne(user);
            return userCollection.Find(x => x.Id == user.Id).FirstOrDefault();
        }

        public UserEntity FindById(Guid id)
        {
            //TODO: Ищи в документации FindXXX
            return userCollection.Find(x => x.Id == id).FirstOrDefault();
        }

        public UserEntity GetOrCreateByLogin(string login)
        {
            //TODO: Это Find или Insert
            var user =  userCollection.Find(x => x.Login == login).FirstOrDefault();
            if (user is null)
            {
                user = new UserEntity(new Guid()) { Login = login };
                user = Insert(user);
            }

            return user;
        }

        public void Update(UserEntity user)
        {
            //TODO: Ищи в документации ReplaceXXX
            userCollection.ReplaceOne(x => x.Id == user.Id, user);
        }

        public void Delete(Guid id)
        {
            userCollection.FindOneAndDelete(x => x.Id == id);
        }

        // Для вывода списка всех пользователей (упорядоченных по логину)
        // страницы нумеруются с единицы
        public PageList<UserEntity> GetPage(int pageNumber, int pageSize)
        {
            //TODO: Тебе понадобятся SortBy, Skip и Limit
            var cursor = userCollection.Find(FilterDefinition<UserEntity>.Empty)
                .ToCursor()
                .ToList();
            var page = cursor
                .OrderBy(user => user.Login)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return new PageList<UserEntity>(page , cursor.Count, pageNumber, pageSize);
        }

        // Не нужно реализовывать этот метод
        public void UpdateOrInsert(UserEntity user, out bool isInserted)
        {
            throw new NotImplementedException();
        }
    }
}