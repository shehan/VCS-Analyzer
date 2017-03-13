using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCSAnalyzer.Entities
{
    class Tag
    {
        string name, message, authorEmail, id;
        DateTime date;

        public Tag(string name, string message, string author, string id, DateTime date)
        {
            this.authorEmail = author;
            this.id = id;
            this.name = name;
            this.message = message;
            this.date = date;
        }

        public string AuthorEmail { get => authorEmail; set => authorEmail = value; }
        public string Id { get => id; set => id = value; }
        public string Message { get => message; set => message = value; }
        public string Name { get => name; set => name = value; }
        public DateTime Date { get => date; set => date = value; }
    }
}
