import axios from "axios";
import { useRouter } from "next/dist/client/router";
import React, { useState } from "react";
import { Button, Columns, Container, Form, Heading, Section } from "react-bulma-components";
import redirect from 'nextjs-redirect'

export default function Login() {


    const router = useRouter()

    const discord_code = router.query.discord_code as string | undefined

    function LoginClicked() {

    }

    async function LoginWithDiscordClicked() {
        const redirectURL = router.query.redirect_url as string | undefined
        let url = process.env.NEXT_PUBLIC_SORIGIN_API + "/api/auth/discord/auth"
        if (redirectURL !== undefined) {
            url += "?redirect_url=" + redirectURL
        }
        window.location.assign(url)
    }
    
    async function SignupWithDiscordClicked() {

    }

    if (discord_code !== undefined) {
        return (<>p</>)
    }
    else {
        return (
            <>
                <Container>
                    <Section>
                        <Columns>
                            <Columns.Column size="one-third">
                            
                            </Columns.Column>
                            <Columns.Column size="one-third">
                                <Heading>Sign In</Heading>
                                <Button color="link" onClick={() => LoginWithDiscordClicked()}>Sign in with Discord</Button>
                            </Columns.Column>
                            <Columns.Column size="one-third">
                            
                            </Columns.Column>
                        </Columns>
                    </Section>
                </Container>
            </>
        )
    }
}