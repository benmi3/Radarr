﻿using Newtonsoft.Json;
using NzbDrone.Core.NetImport.Exceptions;
using NzbDrone.Core.Tv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace NzbDrone.Core.NetImport.IMDbWatchList
{
    public class IMDbWatchListParser : IParseNetImportResponse
    {
        private readonly IMDbWatchListSettings _settings;

        public IMDbWatchListParser(IMDbWatchListSettings settings)
        {
            _settings = settings;
        }

        public IList<Movie> ParseResponse(NetImportResponse netImportResponse)
        {
            var torrentInfos = new List<Movie>();

            if (netImportResponse.HttpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw new NetImportException(netImportResponse,
                    "Unexpected response status {0} code from API request",
                    netImportResponse.HttpResponse.StatusCode);
            }

            var jsonResponse = JsonConvert.DeserializeObject<IMDbWatchListAPI.Channel>(netImportResponse.Content);

            var responseData = jsonResponse.Movie;
            if (responseData == null)
            {
                throw new NetImportException(netImportResponse,
                    "This list has no movies");
            }

            foreach (var result in responseData)
            {
                var title = Parser.Parser.ParseMovieTitle(result.Title, false);

                torrentInfos.Add(new Movie()
                {
                    Title = title.MovieTitle,
                    Year = title.Year,
                    ProfileId = _settings.ProfileId,
                    ImdbId = Parser.Parser.ParseImdbId(result.Link)
                });
            }

            return torrentInfos.ToArray();
        }
    }
}
