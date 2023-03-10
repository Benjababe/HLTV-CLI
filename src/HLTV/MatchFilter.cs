using System.Collections;

namespace HLTV_CLI.src {
    class MatchFilter {
        public bool liveMatch, LAN;
        public int minStars;
        // { [teamID, teamName], [teamID, teamName] }
        public ArrayList teamIDs;

        public MatchFilter(bool liveMatch = false, bool LAN = false, int minStars = 1, ArrayList teamIDs = null) {
            this.liveMatch = liveMatch;
            this.LAN = LAN;
            this.minStars = minStars;
            if (teamIDs == null)
                this.teamIDs = new ArrayList();
            else
                this.teamIDs = teamIDs;
        }
    }
}