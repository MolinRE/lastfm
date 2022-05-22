using System;
using System.Net.Http;
using System.Threading.Tasks;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IF.Lastfm.Core.Api.Commands.User
{
    [ApiMethodName("user.getFriends")]
    internal class GetFriendsCommand : GetAsyncCommandBase<PageResponse<LastUser>>
    {
        public string Username { get; private set; }

        public bool IncludeRecentTrack { get; set; }

        public GetFriendsCommand(ILastAuth auth, string username, bool includeRecentTrack) : base(auth)
        {
            Username = username;
            IncludeRecentTrack = includeRecentTrack;
        }

        public override void SetParameters()
        {
            Parameters.Add("user", Username);
            Parameters.Add("recenttracks", IncludeRecentTrack ? "1" : "0");

            AddPagingParameters();
            DisableCaching();
        }

        public override async Task<PageResponse<LastUser>> HandleResponse(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();

            if (LastFm.IsResponseValid(json, out LastResponseStatus status) && response.IsSuccessStatusCode)
            {
                var jtoken = JsonConvert.DeserializeObject<JToken>(json).SelectToken("friends");
                var itemsToken = jtoken.SelectToken("user");
                var attrToken = jtoken.SelectToken("@attr");

                return PageResponse<LastUser>.CreateSuccessResponse(itemsToken, attrToken, LastUser.ParseJToken, LastPageResultsType.Attr);
            }
            else
            {
                return LastResponse.CreateErrorResponse<PageResponse<LastUser>>(status);
            }
        }
    }
}
