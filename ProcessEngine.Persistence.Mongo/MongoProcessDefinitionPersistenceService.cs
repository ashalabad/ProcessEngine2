﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Components.DictionaryAdapter;
using KlaudWerk.ProcessEngine.Definition;
using KlaudWerk.ProcessEngine.Persistence;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Klaudwerk.ProcessEngine.Persistence.Mongo
{
    public class MongoProcessDefinitionPersistenceService:IProcessDefinitionPersisnenceService
    {
        public const string CollectionName = "process_definitions";
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<ProcessDefinitionPersistence> _collection;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="database">Mondo Database</param>
        public MongoProcessDefinitionPersistenceService(IMongoDatabase database)
        {
            _database = database;
            _collection = _database.GetCollection<ProcessDefinitionPersistence>(CollectionName);
        }

        public IReadOnlyList<ProcessDefinitionDigest> LisAlltWorkflows(params AccountData[] accounts)
        {
            return _collection.Find(p => true).ToList()
            .Select(d => new ProcessDefinitionDigest
            {
                Id = d.Id,
                Description = d.Description,
                Name = d.Name,
                LastUpdated = d.LastModified,
                Md5 = d.Md5,
                Status = (ProcessDefStatusEnum)d.Status,
                Version = d.Version
            }).ToList();
        }

        public IReadOnlyList<ProcessDefinitionDigest> ActivetWorkflows(params AccountData[] accounts)
        {
            var filter =
                Builders<ProcessDefinitionPersistence>.Filter.Eq(r => r.Status, (int) ProcessDefStatusEnum.Active);
            return _collection.Find(filter)
                .ToList()
                .Select(d => new ProcessDefinitionDigest
                {
                    Id = d.Id,
                    Description = d.Description,
                    Name = d.Name,
                    LastUpdated = d.LastModified,
                    Md5 = d.Md5,
                    Status = (ProcessDefStatusEnum) d.Status,
                    Version = d.Version
                })
                .ToList();
        }

        /// <summary>
        /// Ceate process definition
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="status"></param>
        /// <param name="version"></param>
        /// <param name="accounts"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Create(ProcessDefinition definition, ProcessDefStatusEnum status, int version, params AccountData[] accounts)
        {
            Md5CalcVisitor visitor = new Md5CalcVisitor();
            definition.Accept(visitor);
            string md5=visitor.CalculateMd5();
            if (_collection.Find(f => f.FlowId == definition.FlowId && string.Equals(f.Md5, md5)).Count() != 0)
            {
                throw new ArgumentException($"Persistend Workflow definition FlowId={definition.FlowId} Md5={md5} already exists.");
            }
            List<AccountData> accountsList=new EditableList<AccountData>();
            if(accounts!=null)
                accountsList.AddRange(accounts);
            ProcessDefinitionPersistence pd=new ProcessDefinitionPersistence
            {
                Id = definition.Id,
                FlowId=definition.FlowId,
                Version = version,
                Name = definition.Name,
                Description = definition.Description,
                LastModified = DateTime.UtcNow,
                Status = (int)status,
                Md5 = md5,
                JsonProcessDefinition = ToBase64(JsonConvert.SerializeObject(definition)),
                Accounts = accountsList
            };
            _collection.InsertOne(pd);
        }

        /// <summary>
        /// update status
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool SetStatus(Guid id, int version, ProcessDefStatusEnum status)
        {
            var filter = Builders<ProcessDefinitionPersistence>.Filter.And(
                Builders<ProcessDefinitionPersistence>.Filter.Eq(r => r.Id, id),
                Builders<ProcessDefinitionPersistence>.Filter.Eq(r => r.Version, version)
            );
            var update = Builders<ProcessDefinitionPersistence>.Update.Set(r => r.Status, (int) status)
                .CurrentDate(r => r.LastModified);
            UpdateResult result = _collection.UpdateOne(filter, update);
            return result.ModifiedCount ==  1;
        }

        /// <summary>
        /// Implement Fidnd method
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <param name="definition"></param>
        /// <param name="status"></param>
        /// <param name="accounts"></param>
        /// <returns></returns>
        public bool TryFind(Guid id, int version, out ProcessDefinition definition, out ProcessDefStatusEnum status,
            out AccountData[] accounts)
        {
            status=ProcessDefStatusEnum.NotActive;
            definition = null;
            accounts=new AccountData[]{};
            ProcessDefinitionPersistence pd = _collection.Find(r => r.Id == id && r.Version==version).SingleOrDefault();
            if (pd != null)
            {
                definition = JsonConvert.DeserializeObject<ProcessDefinition>(FromBase64(pd.JsonProcessDefinition));
                status = (ProcessDefStatusEnum)pd.Status;
                accounts = pd.Accounts?.ToArray();
            }
            return pd != null;
        }

        /// <summary>
        /// Update the workflow. The only things that can be updated are name and description
        /// </summary>
        /// <param name="id"></param>
        /// <param name="version"></param>
        /// <param name="action"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Update(Guid id, int version, Action<ProcessDefinitionPersistenceBase> action)
        {
            UpdateProcessDefPersistence u=new UpdateProcessDefPersistence();
            action(u);
            var filter = Builders<ProcessDefinitionPersistence>.Filter.And(
                Builders<ProcessDefinitionPersistence>.Filter.Eq(r => r.Id, id),
                Builders<ProcessDefinitionPersistence>.Filter.Eq(r => r.Version, version)
            );
            UpdateDefinition<ProcessDefinitionPersistence> updatedef = null;
            if (u.NameSet)
            {
                updatedef=Builders<ProcessDefinitionPersistence>.Update.Set(r => r.Name, u.Name);
            }
            if (u.DescriptionSet)
            {
                if (updatedef == null)
                    updatedef = Builders<ProcessDefinitionPersistence>.Update.Set(r => r.Description, u.Description);
                else updatedef.Set(r => r.Description, u.Description);
            }
            updatedef = updatedef?.CurrentDate(r => r.LastModified);
            if (updatedef == null)
                return;
            _collection.FindOneAndUpdate(filter, updatedef);
        }

        /// <summary>
        /// Create account records
        /// </summary>
        /// <param name="accounts"></param>
        public void CreateAccounts(params AccountData[] accounts)
        {
        }

        private static string ToBase64(string source)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(source));
        }

        private static string FromBase64(string base64Str)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64Str));
        }

        private class UpdateProcessDefPersistence : ProcessDefinitionPersistenceBase
        {
            public bool NameSet { get; private set; }
            public bool DescriptionSet { get; private set; }

            public override string Name
            {
                get { return base.Name; }
                set
                {
                    base.Name = value;
                    NameSet = true;
                }
            }

            public override string Description
            {
                get { return base.Description; }
                set
                {
                    base.Description = value;
                    DescriptionSet = true;
                }
            }
        }
    }
}