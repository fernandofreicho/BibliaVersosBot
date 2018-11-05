using System;
using System.Collections.Generic;
using System.Text;

namespace BibliaVersosBot.Models
{

    public class Testament
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Book> Books { get; set; }
    }

    public class Book
    {
        public int Id { get; set; }
        public int TestamentId { get; set; }
        public string Abbrev { get; set; }
        public string Name { get; set; }

        public Testament Testament { get; set; }

        public ICollection<Verse> Verses { get; set; }
    }

    public class Verse
    {
        public int Id { get; set; }
        public int TestamentId { get; set; }
        public int BookId { get; set; }
        public int Chapter { get; set; }
        public int VerseNumber { get; set; }
        public string Version { get; set; }
        public string Text { get; set; }

        public Testament Testament { get; set; }
        public Book Book { get; set; }
    }
}
