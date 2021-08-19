<svelte:head>
    <title>Sorigin | Login</title>
</svelte:head>

<script lang="ts">
    import { page } from '$app/stores'
    import { REDIRECT_URL, SORIGIN_URL } from '../../src/utils/env'

    function handleDiscord() {
        let redirectURL = $page.query.get('redirect_url')
        let url = SORIGIN_URL + '/api/auth/discord/auth'
        if (redirectURL === undefined || redirectURL === null) {
            redirectURL = REDIRECT_URL
        }
        if (redirectURL !== undefined) {
            url += '?redirect_url=' + redirectURL
        }
        window.location.assign(url)
    }

</script>

<section class="section">
    <div class="columns">
        <div class="column is-one-third is-offset-one-third">
            <article>
                <h3 class="title">Sign In</h3>
                <button class="button is-primary" on:click={handleDiscord}>
                    Sign in with Discord
                </button>
            </article>
        </div>
    </div>
</section>

<style lang="css" scoped>
    .button.is-primary {
        background-color: #7289DA;
        color: #23272A;
    }
</style>