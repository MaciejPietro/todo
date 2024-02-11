using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace todo.Models
{
    public class NoteModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
