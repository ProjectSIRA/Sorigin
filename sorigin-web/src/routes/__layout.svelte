<svelte:head>
    <meta name="title" content="Sorigin">
    <meta name="description" content="A player-unifying authorization platform for the Beat Saber community.">
    <meta name="keywords" content="beat saber, sorigin">
    <meta name="robots" content="noindex, nofollow">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
    <meta name="language" content="English">
</svelte:head>

<script lang="ts" context="module">
    import type AuthedUser from '$lib/types/authedUser'
    
    export async function load({ session }) {
        return {
            props: {
                user: session.user !== undefined ? session.user : null
            }
        }
    }

</script>

<script lang="ts">
    import { authedUser } from '$lib/stores/usersStore'
    import { onMount } from 'svelte'

    let navOpen: boolean = false
    export let user: AuthedUser | null

    onMount(() => {
        authedUser.set(user)
    })

</script>

<nav class="navbar is-transparent m-4" role="navigation" aria-label="main navigation">
    <div class="navbar-brand">
        <a class="navbar-item subtitle" href="/">
            Sorigin
        </a>
        <div role="button" class="navbar-burger" aria-label="menu" aria-expanded="false" class:is-active="{navOpen}" on:click="{() => navOpen = !navOpen}">
            <span aria-hidden="true"></span>
            <span aria-hidden="true"></span>
            <span aria-hidden="true"></span>
        </div>
    </div>
    <div class="navbar-menu" class:is-active="{navOpen}">
        <div class="navbar-end">
            {#if $authedUser !== null}
                <a class="navbar-item" href="/@{$authedUser.user.username}">
                    <h3 class="subtitle">{$authedUser.user.username}</h3>
                </a>
                <a class="navbar-item" href="/logout">
                    <h3 class="subtitle">Log Out</h3>
                </a>
            {:else}
                <a class="navbar-item subtitle" href="/login">
                    Log In
                </a>
            {/if}
        </div>
    </div>
</nav>

<body>
    <div class="container">
        <slot></slot>
    </div>
</body>

<style lang="scss" global>
    html {
        overflow: auto;
    }

    .navbar {
        background-color: transparent;
    }

    .navbar-menu {
        background-color: transparent;
        box-shadow: none;
    }
</style>