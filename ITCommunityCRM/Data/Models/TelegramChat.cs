using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ITCommunityCRM.Data.Models;

public class TelegramChat
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; set; }
    public string Title { get; set; }
    public bool IsBotJoined { get; set; }
}
