<script lang="ts">
    import type User from '$lib/types/user'

    export let user: User

    function openSteam() {
        if (user.steam !== null) {
            window.open(`https://steamcommunity.com/profiles/${user.steam.id}`, "_blank")
        }
    }

</script>

<button class="button is-primary is-fullwidth" on:click={openSteam}>
    <span class="icon">
        <ion-icon name="logo-steam"></ion-icon>
    </span>
    <span>Steam</span>
</button>

<style scoped>
    .button.is-primary {
        background-color: #14305D;
        color: #cecece;
    }
</style>