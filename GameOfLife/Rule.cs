using System;
using System.Collections.Generic;
using System.Linq;

namespace GameOfLife
{
    public class Rule
    {
        private readonly List<int> _survivalConditions;
        private readonly List<int> _birthConditions;

        public IEnumerable<int> SurvivalConditions { get { return _survivalConditions.AsReadOnly(); } }
        public IEnumerable<int> BirthConditions { get { return _birthConditions.AsReadOnly(); } }

        public Rule(IEnumerable<int> survivalConditions, IEnumerable<int> birthConditions)
        {
            if (survivalConditions == null)
                throw new ArgumentNullException("survivalConditions");
            if (birthConditions == null)
                throw new ArgumentNullException("birthConditions");

            _survivalConditions = survivalConditions.ToList();
            _birthConditions = birthConditions.ToList();
        }

        public bool Birth(int neighbours)
        {
            return _birthConditions.Any(c => c == neighbours);
        }

        public bool Death(int neighbours)
        {
            return !Survive(neighbours);
        }

        public bool Survive(int neightbours)
        {
            return _survivalConditions.Any(c => c == neightbours);
        }
    }
}
