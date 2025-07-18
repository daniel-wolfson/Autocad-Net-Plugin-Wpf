using System;
using System.Collections;

namespace ID.Infrastructure.Models
{
    public class Endpoint
    {
        //
        // Summary:
        //     Creates a new instance of Microsoft.AspNetCore.Http.Endpoint.
        //
        // Parameters:
        //   requestDelegate:
        //     The delegate used to process requests for the endpoint.
        //
        //   metadata:
        //     The endpoint Microsoft.AspNetCore.Http.EndpointMetadataCollection. May be null.
        //
        //   displayName:
        //     The informational display name of the endpoint. May be null.
        public Endpoint(Action requestDelegate, ICollection metadata, string displayName) { }

        //
        // Summary:
        //     Gets the informational display name of this endpoint.
        public string DisplayName { get; }
        //
        // Summary:
        //     Gets the collection of metadata associated with this endpoint.
        public ICollection Metadata { get; }
        //
        // Summary:
        //     Gets the delegate used to process requests for the endpoint.
        public Action RequestDelegate { get; }

        public override string ToString() { return ""; }
    }
}
