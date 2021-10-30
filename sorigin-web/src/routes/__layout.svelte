<script lang="ts">
    import { sessionStore } from '$lib/stores/sessionStore';
    import { page, session } from '$app/stores';
    import { goto } from '$app/navigation';
    import { onMount } from 'svelte';
    import axios from 'axios';

    let navOpen: boolean = false;

    onMount(() => {
        const callback = $page.query.get('callback');

        if (callback) {
            if (window) {
                window.location.href = '/';
            }
        } else {
            sessionStore.set($session.tokens ? $session : null);
        }
    });

    async function logOut() {
        sessionStore.set(null);
        await axios.delete('/session');
    }
</script>

<nav class="navbar is-transparent m-4" role="navigation" aria-label="main navigation">
    <div class="navbar-brand">
        <a class="navbar-item subtitle" href="/"> Sorigin </a>
        <div
            role="button"
            class="navbar-burger"
            aria-label="menu"
            aria-expanded="false"
            class:is-active={navOpen}
            on:click={() => (navOpen = !navOpen)}
        >
            <span aria-hidden="true" />
            <span aria-hidden="true" />
            <span aria-hidden="true" />
        </div>
    </div>
    <div class="navbar-menu" class:is-active={navOpen}>
        <div class="navbar-end">
            {#if $sessionStore !== null}
                <a class="navbar-item" href="/@{$sessionStore.user.username}">
                    <h3 class="subtitle">{$sessionStore.user.username}</h3>
                </a>
                <!-- svelte-ignore a11y-missing-attribute -->
                <a class="navbar-item" on:click={logOut}>
                    <h3 class="subtitle">Log Out</h3>
                </a>
            {:else}
                <a class="navbar-item subtitle" href="/login"> Log In </a>
            {/if}
        </div>
    </div>
</nav>

<body>
    <div class="container">
        <slot />
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

    [data-tooltip] {
        &:not(a):not(button):not(input) {
            border-bottom: 0px;
        }
    }

    ion-icon {
        pointer-events: none;
    }
</style>
