<svelte:head>
    <title>Sorigin</title>
</svelte:head>

<script lang="ts" context="module">
    import { SORIGIN_URL } from '../utils/env'
    import { authedUser } from '$lib/stores/usersStore'
    import type AuthedUser from '$lib/types/authedUser'
    import type User from '$lib/types/user'

    export async function load({ page, fetch }) {
        const grant = page.query.get('grant')

        try {
            if (grant !== null && grant !== undefined) {
            
                const url = `${SORIGIN_URL}/api/auth/token`
                const res = await fetch(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({
                        platform: 0,
                        token: grant 
                    })
                })
                if (res.ok) {
                    const body = await res.json()
                    const token = body.token

                    const userRes = await fetch(`${SORIGIN_URL}/api/auth/@me`, {
                        method: 'GET',
                        headers: {
                            'Authorization': `Bearer ${token}`
                        }
                    })

                    if (userRes.ok) {
                        const user: User = (await userRes.json())
                        authedUser.set({ token, user } as AuthedUser)
                    }
                }
            }
        }
        catch {

        }


        return {
            props: {

            }
        }
    }

</script>


<article>
    <h3>What is Sorigin?</h3>
    <p>
        Sorigin is a service which aims to unify player identification in the Beat Saber community.
        Some services and platforms need to link a player to different social platforms like Discord and Steam.
        However, it can be cumbersome for a service to handle authenticating users with all those platforms,
        and it can be annoying for users to have to log into multiple platforms.
    </p>
</article>