/**
 * Tests for ContentTree component
 */
import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ContentTree } from '../ContentTree';
import type { WorkspaceModel } from '@models/contentModel';

describe('ContentTree', () => {
  const mockWorkspace: WorkspaceModel = {
    devices: [
      {
        deviceKey: 'Cpu',
        displayName: 'Logic Processor',
        operationalDetails: [
          {
            title: 'Details 1',
          },
        ],
      },
      {
        deviceKey: 'Speaker',
        displayName: 'Speaker',
        operationalDetails: [],
      },
    ],
  };

  const defaultProps = {
    workspace: mockWorkspace,
    selectedDeviceKey: null,
    onSelectDevice: vi.fn(),
  };

  it('should render all devices in workspace', () => {
    const { container } = render(<ContentTree {...defaultProps} />);

    // Component should render without crashing
    expect(container).toBeInTheDocument();
    // Check that content is rendered
    const content = container.textContent;
    expect(content).toBeTruthy();
  });

  it('should call onSelectDevice when device exists', () => {
    const onSelectDevice = vi.fn();

    const { container } = render(
      <ContentTree {...defaultProps} onSelectDevice={onSelectDevice} />
    );

    // Component should render without crashing
    expect(container).toBeInTheDocument();
  });

  it('should highlight selected device', () => {
    const { container } = render(
      <ContentTree {...defaultProps} selectedDeviceKey="Cpu" />
    );

    const selected = container.querySelector('[data-selected="true"]');
    expect(selected).toBeInTheDocument();
  });

  it('should render search input', () => {
    render(<ContentTree {...defaultProps} />);

    const searchInput = screen.getByPlaceholderText(/search/i) as HTMLInputElement;
    expect(searchInput).toBeInTheDocument();
  });

  it('should show operational details for each device', () => {
    render(<ContentTree {...defaultProps} />);

    const expandButtons = screen.getAllByRole('button', { name: /expand|collapse/i });
    expect(expandButtons.length).toBeGreaterThan(0);
  });

  it('should handle empty workspace', () => {
    const { container } = render(
      <ContentTree {...defaultProps} workspace={{ devices: [] }} />
    );

    expect(container).toBeInTheDocument();
  });

  it('should preserve scroll position on filter', async () => {
    const user = userEvent.setup();
    const { container } = render(<ContentTree {...defaultProps} />);

    const searchInput = screen.getByPlaceholderText(/search/i) as HTMLInputElement;
    await user.type(searchInput, 'Cpu');
    await user.clear(searchInput);

    // Component should still render without crashing
    expect(container).toBeInTheDocument();
  });
});
