import { describe, it, expect, afterEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import DemoBanner from './DemoBanner';

// Bygger en JWT med given payload. Signaturen spelar ingen roll – frontend avkodar bara payload.
const fakeToken = (payload) =>
  `header.${btoa(JSON.stringify(payload))}.signature`;

describe('DemoBanner', () => {
  afterEach(() => {
    localStorage.clear();
  });

  it('visas med länk till koden när token kommer från en demo', () => {
    // Arrange
    localStorage.setItem('token', fakeToken({ IsDemo: 'true' }));

    // Act
    render(<DemoBanner />);

    // Assert
    expect(screen.getByText(/Du testar en demo/)).toBeTruthy();
    expect(
      screen
        .getByRole('link', { name: /Se koden på GitHub/ })
        .getAttribute('href'),
    ).toBe('https://github.com/Burra17/ShiftMate');
  });

  it('visas inte för vanliga användare', () => {
    // Arrange
    localStorage.setItem('token', fakeToken({ OrganizationName: 'Riktig' }));

    // Act
    render(<DemoBanner />);

    // Assert
    expect(screen.queryByText(/Du testar en demo/)).toBeNull();
  });
});
