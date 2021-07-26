import React from 'react'
import Image from 'next/image'
import { Button, Container, Navbar } from 'react-bulma-components'

export default function SoriginNavbar() {
  return (
    <Navbar shadowless={false}>
      <Container>
        <Navbar.Brand>
          <Navbar.Item href="#">
            <Image
              alt="Bulma: a modern CSS framework based on Flexbox"
              height="28"
              src="https://bulma.io/images/bulma-logo.png"
              width="112"
            />
          </Navbar.Item>
          <Navbar.Burger />
        </Navbar.Brand>
        <Navbar.Menu>
          <Navbar.Container>
            <Navbar.Item href="#">
              Home
            </Navbar.Item>
          </Navbar.Container>
          <Navbar.Container align="right">
            <Button.Group>
              <Button color="primary">Auros</Button>
              <Button>Logout</Button>
            </Button.Group>
          </Navbar.Container>
        </Navbar.Menu>
      </Container>
    </Navbar>
  )
}