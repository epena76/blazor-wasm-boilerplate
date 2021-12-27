﻿using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Shared.Authorization;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace FSH.BlazorWebAssembly.Client.Pages.Identity.Roles;

public partial class Roles
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;

    [Inject]
    private IRolesClient RolesClient { get; set; } = default!;

    protected EntityClientTableContext<RoleDto, string?> Context { get; set; } = default!;

    private bool _canViewRoleClaims;

    protected bool CheckBox { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canViewRoleClaims = (await AuthService.AuthorizeAsync(state.User, FSHPermissions.RoleClaims.View)).Succeeded;

        Context = new(
            fields: new()
            {
                new("Id", L["Id"], role => role.Id),
                new("Name", L["Name"], role => role.Name),
                new("Description", L["Description"], role => role.Description),
            },
            idFunc: role => role.Id,
            loadDataFunc: async () => (await RolesClient.GetListAsync()).Adapt<ListResult<RoleDto>>(),
            searchFunc: Search,
            createFunc: async role => await RolesClient.RegisterRoleAsync(role.Adapt<RoleRequest>()),
            updateFunc: async role => await RolesClient.RegisterRoleAsync(role.Adapt<RoleRequest>()),
            deleteFunc: async id => await RolesClient.DeleteAsync(id),
            entityName: L["Role"],
            entityNamePlural: L["Roles"],
            searchPermission: FSHPermissions.Roles.ListAll,
            createPermission: FSHPermissions.Roles.Register,
            updatePermission: FSHPermissions.Roles.Update,
            deletePermission: FSHPermissions.Roles.Remove,
            hasExtraActionsFunc: () => _canViewRoleClaims);
    }

    private bool Search(string? searchString, RoleDto role) =>
        string.IsNullOrWhiteSpace(searchString)
        || role.Name?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true
        || role.Description?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true;

    private void ManagePermissions(string? roleId)
    {
        ArgumentNullException.ThrowIfNull(roleId, nameof(roleId));
        _navigationManager.NavigateTo($"/identity/role-permissions/{roleId}");
    }
}