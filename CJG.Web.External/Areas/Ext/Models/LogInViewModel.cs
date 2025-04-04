﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CJG.Web.External.Areas.Ext.Models
{
    /// <summary>
    /// <typeparamref name="LogInViewModel"/> class, provides a View Model for logging into the application.
    /// </summary>
    public class LogInViewModel
    {
	    public IList<KeyValuePair<string, string>> Users { get; set; }

        [Required]
        public string SelectedUser { get; set; }

        public LogInViewModel()
        {
        }

        public LogInViewModel(IList<KeyValuePair<string, string>> users)
        {
            Users = users;
        }
    }
}