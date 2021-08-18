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

<button id="so-discord-button" on:click={toggleDiscord}>{buttonText}</button>

<style scoped>
    #so-discord-button {
        --primary: #7289DA ;
        --primary-hover: #4E5D94;
        --primary-focus: rgba(35, 39, 42, 0.125);
        --primary-inverse: #23272A;
    }
</style>