using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Collisions
{
    internal class BatchingProcessor
    {
        public Dictionary<int, Dictionary<int, List<Contact>>> contacts = new Dictionary<int, Dictionary<int, List<Contact>>>();
        public List<ContactBatch> batches = new List<ContactBatch>();
        private Dictionary<int, int> batchLocations = new Dictionary<int, int>();

        public void AddContact(Contact c)
        {
            var id0 = c.gameObject[0].GetInstanceID();
            var id1 = c.gameObject[1].GetInstanceID();
            if (batchLocations.ContainsKey(id0))
            {
                int i;
                batchLocations.TryGetValue(id0, out i);
                batches[i].AddContact(c);
                if (!batchLocations.ContainsKey(id1))
                    batchLocations.Add(id1, i);
            }
            else if (batchLocations.ContainsKey(id1))
            {
                int i;
                batchLocations.TryGetValue(id1, out i);
                batches[i].AddContact(c);
                batchLocations.Add(id0, i);
            }
            else
            {
                ContactBatch cb = new ContactBatch();
                cb.AddContact(c);
                batches.Add(cb);
                batchLocations.Add(id0, batches.Count - 1);
                batchLocations.Add(id1, batches.Count - 1);
            }

            //int id0 = c.gameObject[0].GetInstanceID();
            //if (!contacts.ContainsKey(id0))
            //{
            //    contacts.Add(id0, new Dictionary<int, List<Contact>>());
            //}

            //Dictionary<int, List<Contact>> colliders;
            //contacts.TryGetValue(id0, out colliders);
            //int id1 = c.gameObject[1].GetInstanceID();

            //if (!colliders.ContainsKey(id1))
            //{
            //    colliders.Add(id1, new List<Contact>());
            //}

            //List<Contact> contactList;
            //colliders.TryGetValue(id1, out contactList);
            //contactList.Add(c);
        }

        public void AddContacts(List<Contact> cList)
        {
            foreach (Contact c in cList)
            {
                AddContact(c);
            }
        }

        public void BatchContacts()
        {
            foreach (KeyValuePair<int, Dictionary<int, List<Contact>>> kv in contacts)
            {
                foreach (KeyValuePair<int, List<Contact>> innerKv in kv.Value)
                {
                    if (batchLocations.ContainsKey(kv.Key))
                    {
                        int i;
                        batchLocations.TryGetValue(kv.Key, out i);
                        batches[i].AddContacts(innerKv.Value);
                        if (!batchLocations.ContainsKey(innerKv.Key))
                            batchLocations.Add(innerKv.Key, i);
                    }
                    else if (batchLocations.ContainsKey(innerKv.Key))
                    {
                        int i;
                        batchLocations.TryGetValue(innerKv.Key, out i);
                        batches[i].AddContacts(innerKv.Value);
                        batchLocations.Add(kv.Key, i);
                    }
                    else
                    {
                        ContactBatch cb = new ContactBatch();
                        cb.AddContacts(innerKv.Value);
                        batches.Add(cb);
                        batchLocations.Add(kv.Key, batches.Count - 1);
                        batchLocations.Add(innerKv.Key, batches.Count - 1);
                    }
                }
            }
        }

        public void Clear()
        {
            contacts = new Dictionary<int, Dictionary<int, List<Contact>>>();
            batches = new List<ContactBatch>();
            batchLocations = new Dictionary<int, int>();
        }
    }
}