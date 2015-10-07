using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Collisions
{
    internal class ContactBatch
    {
        private Dictionary<string, Contact> contacts = new Dictionary<string, Contact>();

        public bool HasContact(Contact c)
        {
            return contacts.ContainsKey(c.UniqueId());
        }

        public Contact[] GetAllContacts()
        {
            IEnumerable<Contact> res = contacts.Values;
            return res.ToArray();
        }

        public void AddContact(Contact c)
        {
            if (HasContact(c)) return;
            contacts.Add(c.UniqueId(), c);
        }
        public void AddContacts(List<Contact> cList)
        {
            foreach (Contact c in cList)
                AddContact(c);
        }
    }
}
