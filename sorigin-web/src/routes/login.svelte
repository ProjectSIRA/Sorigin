<svelte:head>
    <title>Sorigin | Login</title>
</svelte:head>

<script lang="ts">
    import { page } from '$app/stores'
    import { SORIGIN_URL } from '../../src/utils/env'

    function handleDiscord() {
        console.log(SORIGIN_URL)
        const redirectURL = $page.query.get('redirect_url')
        let url = SORIGIN_URL + '/api/auth/discord/auth'
        if (redirectURL !== undefined) {
            url += '?redirect_url=' + redirectURL
        }
        window.location.assign(url)
    }

</script>

<div class="grid">
    <div></div>
    <div>
        <article>
            <h3>Sign In</h3>
            <button on:click={handleDiscord}>
                Sign in with Discord
            </button>
        </article>
    </div>
    <div></div>
</div>

<style lang="css">
    :root:not([data-theme="dark"]) {
        --primary: #7289DA ;
        --primary-hover: #4E5D94;
        --primary-focus: rgba(35, 39, 42, 0.125);
        --primary-inverse: #23272A;
    }

    @media only screen and (prefers-color-scheme: dark) {
        :root:not([data-theme="light"]) {
            --primary: #7289DA;
            --primary-hover: #4E5D94;
            --primary-focus: rgba(35, 39, 42, 0.125);
            --primary-inverse: #23272A;
        }
    }
</style>