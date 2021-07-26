import Head from 'next/head'
import React from 'react'
import { Container, Content, Section } from 'react-bulma-components'
import SoriginNavbar from '../components/SoriginNavbar'

export default function Home() {
  return (
    <div>
      <Head>
        <title>Sorigin</title>
        <meta name="description" content="A player information unifier for the Beat Saber Community" />
        <link rel="icon" href="/favicon.ico" />
      </Head>
      <Container>
        <Section>
          <Content>
            <h2>What is Sorigin?</h2>
            <p>
              Sorigin is a service which aims to unify player identification in the Beat Saber community.
              Some services and platforms need to link a player to different social platforms like Discord and Steam.
              However, it can be cumbersome for a service to handle authenticating users with all those platforms, and
              it can be annoying for users to have to log into multiple platforms.  
            </p>
            <h2>Supported Platforms</h2>
            <ul>Discord</ul>
            <ul>Steam</ul>
            <h2>Platforms Coming Soon</h2>
            <ul>Twitch</ul>
            <ul>Twitter</ul>
            <ul>YouTube</ul>
            <h2>Pseudo-Platforms Coming Soon</h2>
            <ul>BeatSaver</ul>
            <ul>Oculus</ul>
            <h2>Features Coming Soon</h2>
            <ul>Sign Up Page</ul>
            <ul>Player Profile Page</ul>
          </Content>
        </Section>
      </Container>
    </div>
  )
}
