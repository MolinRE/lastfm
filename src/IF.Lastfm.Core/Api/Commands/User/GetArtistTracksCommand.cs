﻿using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace IF.Lastfm.Core.Api.Commands.User
{
    [ApiMethodName("user.getArtistTracks")]
    internal class GetArtistTracksCommand : GetAsyncCommandBase<PageResponse<LastTrack>>
    {
        public string Username { get; private set; }

        public string ArtistName { get; private set; }

        public DateTimeOffset? From { get; set; }

        public DateTimeOffset? To { get; set; }

        public GetArtistTracksCommand(ILastAuth auth, string username, string artistName) : base(auth)
        {
            Username = username;
            ArtistName = artistName;
        }

        public override void SetParameters()
        {
            Parameters.Add("user", Username);
            Parameters.Add("artist", ArtistName);

            if (From.HasValue)
            {
                Parameters.Add("startTimestamp", From.Value.AsUnixTime().ToString());
            }

            if (To.HasValue)
            {
                Parameters.Add("endTimestamp", To.Value.AsUnixTime().ToString());
            }

            AddPagingParameters();
            DisableCaching();
        }

        public override async Task<PageResponse<LastTrack>> HandleResponse(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();

            LastResponseStatus status;
            if (LastFm.IsResponseValid(json, out status) && response.IsSuccessStatusCode)
            {
                var jtoken = JsonConvert.DeserializeObject<JToken>(json).SelectToken("artisttracks");
                var itemsToken = jtoken.SelectToken("track");
                var attrToken = jtoken.SelectToken("@attr");

                return PageResponse<LastTrack>.CreateSuccessResponse(itemsToken, attrToken, LastTrack.ParseJToken, LastPageResultsType.Attr);
            }
            else
            {
                return LastResponse.CreateErrorResponse<PageResponse<LastTrack>>(status);
            }
        }
    }
}
