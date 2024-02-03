using System;
using System.ComponentModel.DataAnnotations;

namespace AndroidStorageAccessor.Model
{
    public class FileItem
    {
        [Key]
        public int Id { get; set; }

        public string FilePath { get; set; }
        public bool IsDirectory { get; set; }
    }
}
