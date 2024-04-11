using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusikMacher
{
  // represents one track file
  public class Track
  {

    public Track(string name, string path, DateTime creationTime, string tag)
    {
      this.name = name;
      this.creationTime = creationTime;
      this.path = path;
      this.tag = tag;
    }

    public string path { get; set; }
    [Key]
    public string name { get; set; }
    public string tag { get; set; }
    public DateTime creationTime { get; set; }
  }
}
