<script lang="ts" context="module">
    import type User from '$lib/types/user'
    import { REFETCH_URL, SORIGIN_URL } from '../utils/env'
    import { browser } from '$app/env'

    export const ssr = true

    export async function load({ page, fetch }) {
        const backend = browser ? SORIGIN_URL : REFETCH_URL
        const url = `${backend}/api/user/by-username/${page.params.username}`
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

<section class="section">
    <div class="columns">
        <div class="column is-one-quarter">
            <figure class="image is-256x256">
                <img id="so-curved" src={getPFP(user, Size.Medium)} alt="User Profile" />
            </figure>
        </div>
        <div class="column">
            <div class="content">
                <h1 class="title">{user.username}</h1>
                {#if user.bio !== null}
                    <p>{user.bio}</p>
                {:else}
                    <p>We don't know much about {user.username}... but we bet they're cool!</p>
                {/if}
            </div>
        </div>
        <div class="column is-one-fifth">
            <div class="buttons are-large">
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
    </div>
</section>

<style>
    #so-curved {
        border-radius: 10%;
    }
</style>