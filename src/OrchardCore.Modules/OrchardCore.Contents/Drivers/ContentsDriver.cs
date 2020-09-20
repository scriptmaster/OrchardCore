using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents.Core;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Drivers
{
    public class ContentsDriver : ContentDisplayDriver
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContentsDriver(
            IContentDefinitionManager contentDefinitionManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<IDisplayResult> DisplayAsync(ContentItem model, IUpdateModel updater)
        {
            // We add custom alternates. This could be done generically to all shapes coming from ContentDisplayDriver but right now it's
            // only necessary on this shape. Otherwise c.f. ContentPartDisplayDriver

            var context = _httpContextAccessor.HttpContext;
            var results = new List<IDisplayResult>();
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(model.ContentType);
            var contentsMetadataShape = Shape("ContentsMetadata", new ContentItemViewModel(model)).Location("Detail", "Content:before");

            if (contentTypeDefinition != null)
            {
                contentsMetadataShape.Displaying(ctx =>
                {
                    var stereotype = "";
                    var settings = contentTypeDefinition?.GetSettings<ContentTypeSettings>();
                    if (settings != null)
                    {
                        stereotype = settings.Stereotype;
                    }

                    if (!String.IsNullOrEmpty(stereotype) && !String.Equals("Content", stereotype, StringComparison.OrdinalIgnoreCase))
                    {
                        ctx.Shape.Metadata.Alternates.Add($"{stereotype}__ContentsMetadata");
                    }
                });

                results.Add(contentsMetadataShape);

                var hasEditPermission = await ContentTypeAuthorizationHelper.AuthorizeDynamicPermissionAsync(context, Permissions.EditContent, model);

                if (hasEditPermission)
                {
                    results.Add(Shape("Contents_SummaryAdmin__Button__Edit", new ContentItemViewModel(model)).Location("SummaryAdmin", "Actions:10"));
                }

                var hasPublishPermission = await ContentTypeAuthorizationHelper.AuthorizeDynamicPermissionAsync(context, Permissions.PublishContent, model);

                if (hasPublishPermission)
                {
                    results.Add(Shape("Contents_SummaryAdmin__Button__Actions", new ContentItemViewModel(model)).Location("SummaryAdmin", "ActionsMenu:10"));
                }
            }

            results.Add(Shape("Contents_SummaryAdmin__Tags", new ContentItemViewModel(model)).Location("SummaryAdmin", "Tags:10"));
            results.Add(Shape("Contents_SummaryAdmin__Meta", new ContentItemViewModel(model)).Location("SummaryAdmin", "Meta:20"));

            return Combine(results.ToArray());
        }

        public override async Task<IDisplayResult> EditAsync(ContentItem contentItem, IUpdateModel updater)
        {
            var context = _httpContextAccessor.HttpContext;
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);
            var results = new List<IDisplayResult>();

            if (contentTypeDefinition == null)
            {
                return null;
            }

            var hasPublishPermission = await ContentTypeAuthorizationHelper.AuthorizeDynamicPermissionAsync(context, Permissions.PublishContent, contentItem);

            if (hasPublishPermission)
            {
                results.Add(Dynamic("Content_PublishButton").Location("Actions:10"));
            }

            var hasEditPermission = await ContentTypeAuthorizationHelper.AuthorizeDynamicPermissionAsync(context, Permissions.EditContent, contentItem);

            if (hasEditPermission)
            {
                if (contentTypeDefinition.GetSettings<ContentTypeSettings>().Draftable)
                {
                    results.Add(Dynamic("Content_SaveDraftButton").Location("Actions:20"));
                }
            }

            return Combine(results.ToArray());
        }
    }
}
