using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace AppLearningEnglish.Models.ViewModels
{
    public class VocabularyViewModel
    {
        public int TotalWords { get; set; }

        public int MasteredWords { get; set; }

        public int ReviewWords { get; set; }

        public IEnumerable<UserVocabulary>Words { get; set; }= new List<UserVocabulary>();
    }
}