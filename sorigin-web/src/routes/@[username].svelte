<script lang="ts" context="module">
    import type User from '../types/user'
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
    import { GamePlatform } from '../types/user'
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
        <p>We don't know much about Auros... but we bet they're cool!</p>
    </div>
    <div class="col-xs-2">
        {#if user.steam !== null}
            <button>Steam</button>
        {/if}
        {#if user.discord !== null}
            <button>Discord</button>
        {/if}
        {#if user.gamePlatform === GamePlatform.Steam}
            <button>ScoreSaber</button>
        {/if}
    </div>
</div>

<style>
    #so-curved {
        border-radius: 10%;
    }
</style>