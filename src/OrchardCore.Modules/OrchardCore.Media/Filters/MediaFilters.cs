using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;

namespace OrchardCore.Media.Filters
{
    public class MediaUrlFilter : ILiquidFilter
    {
        private readonly IMediaFileStore _mediaFileStore;

        public MediaUrlFilter(IMediaFileStore mediaFileStore)
        {
            _mediaFileStore = mediaFileStore;
        }

        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var url = input.ToStringValue();
            var imageUrl = _mediaFileStore.MapPathToPublicUrl(url);

            return new ValueTask<FluidValue>(new StringValue(imageUrl ?? url));
        }
    }

    public class ImageTagFilter : ILiquidFilter
    {
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var url = input.ToStringValue();

            var imgTag = $"<img src=\"{url}\"";

            foreach (var name in arguments.Names)
            {
                imgTag += $" {name.Replace('_', '-')}=\"{arguments[name].ToStringValue()}\"";
            }

            imgTag += " />";

            return new ValueTask<FluidValue>(new StringValue(imgTag) { Encode = false });
        }
    }

    public class ResizeUrlFilter : ILiquidFilter
    {
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var url = input.ToStringValue();

            var queryStringParams = new Dictionary<string, string>();

            var width = arguments["width"].Or(arguments.At(0));
            var height = arguments["height"].Or(arguments.At(1));
            var mode = arguments["mode"].Or(arguments.At(2));
            var quality = arguments["quality"];//.Or(arguments.At(3));
            var format = arguments["format"];//.Or(arguments.At(4));
            var center = arguments["center"];

            if (!width.IsNil())
            {
                queryStringParams.Add("width", width.ToStringValue());
            }

            if (!height.IsNil())
            {
                queryStringParams.Add("height", height.ToStringValue());
            }

            if (!mode.IsNil())
            {
                queryStringParams.Add("rmode", mode.ToStringValue());
            }

            if (!quality.IsNil())
            {
                queryStringParams.Add("quality", quality.ToStringValue());
            }

            if (!format.IsNil())
            {
                queryStringParams.Add("format", format.ToStringValue());
            }

            if (!center.IsNil() && center.Type == FluidValues.Array)
            {
                var xy = String.Empty;
                foreach (var value in center.Enumerate())
                {
                    if (!value.IsNil())
                    {
                        xy = String.Empty;
                        break;
                    }
                    xy = xy + value.ToNumberValue().ToString() + ',';
                }
                if (!String.IsNullOrEmpty(xy))
                {
                    queryStringParams.Add("rxy", xy.Trim(','));
                }
            }

            return new ValueTask<FluidValue>(new StringValue(QueryHelpers.AddQueryString(url, queryStringParams)));
        }
    }
}
