using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piipan.Shared.API.Enums
{
    public enum SearchMatchStatus
    {
        [Display(Name = "New Created Match")]
        NEWMATCH,
        [Display(Name = "Already Existing Match")]
        EXISTINGMATCH,
        [Display(Name = "No Match Found")]
        MATCHNOTFOUND
    }
}
