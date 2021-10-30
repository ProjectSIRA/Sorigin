<script lang="ts">
    import type User from '$lib/types/user';
    import { Role } from '$lib/types/user';

    export let user: User;

    interface RoleInfo {
        id: string;
        icon: string;
        name: string;
    }

    const roleData: RoleInfo | null = roleInfo();

    function roleInfo() {
        if ((user.role & Role.Owner) === Role.Owner)
            return { id: 'owner', icon: 'build', name: 'Owner of Sorigin' } as RoleInfo;
        if ((user.role & Role.Admin) === Role.Admin)
            return { id: 'admin', icon: 'shield-checkmark', name: 'Sorigin Admin' } as RoleInfo;
        if ((user.role & Role.Verified) === Role.Verified)
            return {
                id: 'verified',
                icon: 'checkmark-circle',
                name: 'Verified Account'
            } as RoleInfo;
        return null;
    }
</script>

{#if roleData !== null}
    <span class="icon-text has-text-info">
        <span class="icon is-large" data-tooltip={roleData.name} id={roleData.id}>
            <ion-icon name={roleData.icon} size="large" />
        </span>
        <span>
            <h1 class="title">{user.username}</h1>
        </span>
    </span>
{:else}
    <h1 class="title">{user.username}</h1>
{/if}

<style scoped>
    #owner {
        color: #3affff;
    }

    #admin {
        color: #ff3a3a;
    }

    #verified {
        color: #cafffc;
    }
</style>
