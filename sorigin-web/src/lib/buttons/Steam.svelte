<script lang="ts">
    import type User from '$lib/types/user'

    export let user: User

    function openSteam() {
        if (user.steam !== null) {
            window.open(`https://steamcommunity.com/profiles/${user.steam.id}`, "_blank")
        }
    }

</script>

<button id="so-steam-button" on:click={openSteam}>Steam</button>

<style scoped>
    #so-steam-button {
        --primary: #14305D;
        --primary-hover: #040e1d;
        --primary-focus: rgba(20, 48, 93, 0.25);
        --primary-inverse: rgb(209, 209, 209);
    }
</style>