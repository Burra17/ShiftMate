import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import EmptyState from './EmptyState';

describe('EmptyState', () => {
  it('visar meddelandet', () => {
    // Arrange + Act
    render(<EmptyState message="Inga pass hittades" />);

    // Assert – getByText kastar fel om texten inte finns
    expect(screen.getByText('Inga pass hittades')).toBeTruthy();
  });

  it('visar länk när linkTo och linkText anges', () => {
    // EmptyState använder <Link>, som kräver en router runt sig
    render(
      <MemoryRouter>
        <EmptyState
          message="Tomt"
          linkTo="/marketplace"
          linkText="Hitta pass"
        />
      </MemoryRouter>,
    );

    expect(
      screen.getByRole('link', { name: 'Hitta pass' }).getAttribute('href'),
    ).toBe('/marketplace');
  });
});
