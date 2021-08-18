<script lang="ts" context="module">
    import type User from '$lib/types/user'
    import { SORIGIN_URL } from '../utils/env'

    /**
	* @type {import('@sveltejs/kit').Load}
	*/
    export async function load({ page, fetch }) {
        const url = `${SORIGIN_URL}/api/user/by-username/${page.params.username}`
        const res = await fetch(url)

        if (res.ok) {
            const user: User = await res.json()
            return { props: { user } }
        }

        return {
            status: res.status,
            error: new Error(`Could not load ${url}`)
        }
    }

</script>

<script lang="ts">
    import ScoreSaber from '$lib/buttons/ScoreSaber.svelte'
    import Discord from '$lib/buttons/Discord.svelte'
    import Steam from '$lib/buttons/Steam.svelte'
    import { GamePlatform } from '$lib/types/user'
    import { getPFP, Size } from '../utils/users'
    export let user: User
</script>

<svelte:head>
    <title>Sorigin | {user.username}'s Profile</title>
</svelte:head>

<div class="row">
    <div class="col-xs-3">
        <img id="so-curved" src={getPFP(user, Size.Medium)} alt="User Profile" />
    </div>
    <div class="col-xs-7">
        <h1>{user.username}</h1>
        {#if user.bio !== null}
            <p>{user.bio}</p>
        {:else}
            <p>We don't know much about {user.username}... but we bet they're cool!</p>
        {/if}
    </div>
    <div class="col-xs-2">
        {#if user.steam !== null}
            <Steam user={user}></Steam>
        {/if}
        {#if user.discord !== null}
            <Discord user={user}></Discord>
        {/if}
        {#if user.gamePlatform === GamePlatform.Steam}
            <ScoreSaber user={user}></ScoreSaber>
        {/if}
    </div>
</div>

<style>
    #so-curved {
        border-radius: 10%;
    }
</style>