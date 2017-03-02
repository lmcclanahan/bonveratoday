using Backoffice.Models;
using ExigoService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Backoffice.ViewModels
{
    public class TreeViewerViewModel
    {
        public TreeViewerViewModel()
        {
            this.Ranks = new List<Rank>();
        }

        public List<Rank> Ranks { get; set; }

    }
}