<script lang="ts">
    import type User from '$lib/types/user'

    let buttonText = 'Discord'
    export let user: User

    function toggleDiscord() {
        if (user.discord !== null) {
            if (buttonText === 'Discord') {
                buttonText = `${user.discord.username}#${user.discord.discriminator}`
            }
            else {
                buttonText = 'Discord'
            }
        }
    }

</script>

<button class="button is-primary is-fullwidth" on:click={toggleDiscord}>
    <span class="icon">
        <ion-icon name="logo-discord"></ion-icon>
    </span>
    <span>{buttonText}</span>
</button>

<style scoped>
    .button.is-primary {
        background-color: #7289DA;
        color: #23272A;
    }
</style>