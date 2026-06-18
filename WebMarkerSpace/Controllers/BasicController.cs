using Microsoft.AspNetCore.Mvc;
using Infrastructure;
using Infrastructure.NHibernate;
using NHibernate; // Mantener para otros usos
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Alias para NHibernate.ISession
using NHibernateSession = NHibernate.ISession;

namespace WebMarkerSpace.Controllers
{
    public class BasicController : Controller
    {
        private NHibernateSession sessionInside;

        protected SessionCPNHibernate session;

        protected BasicController()
        {
        }

        protected void SessionInitialize()
        {
            if (session == null)
            {
                sessionInside = NHibernateHelper.BuildSessionFactory().OpenSession();
                session = new SessionCPNHibernate(sessionInside);
            }
        }

        protected void SessionClose()
        {
            if (session != null && sessionInside.IsOpen)
            {
                sessionInside.Close();
                sessionInside.Dispose();
                session = null;
            }
        }
    }
}


namespace Infrastructure.NHibernate
{
    public class SessionCPNHibernate
    {
        private readonly NHibernateSession _session;

        public SessionCPNHibernate(NHibernateSession session)
        {
            _session = session;
        }
    }
}





