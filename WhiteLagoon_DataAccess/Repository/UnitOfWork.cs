﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteLagoon_DataAccess.Repository.IRepository;

namespace WhiteLagoon_DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public IVillaRepository Villa { get; private set; }
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            Villa = new VillaRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}