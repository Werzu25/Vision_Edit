using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using Models;

namespace Models;
public class DocumentModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Content { get; set; }
    public DateTime CreationDate { get; set; }
    public UserModel User { get; set; }
}